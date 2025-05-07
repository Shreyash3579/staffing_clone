using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Contracts.Services;
using Hcpd.API.Core.Helpers;
using Hcpd.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hcpd.API.Core.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }
        public async Task<IEnumerable<ReviewViewModel>> GetReviewsByEmployeeCode(string employeeCode)
        {
            var reviewsData = await _reviewRepository.GetReviewsByEmployeeCode(employeeCode);
            var reviews = ConvertToReviewViewModel(reviewsData);
            ExtractSkillplanFromReview(reviews);
            return reviews;
        }

        private void ExtractSkillplanFromReview(IEnumerable<ReviewViewModel> reviews)
        {
            foreach (var review in reviews)
            {
                XmlDocument xmlReview = LoadReviewData(review);
                var nodeRoot = xmlReview.DocumentElement;

                var reviewDocVersion = int.Parse(nodeRoot.SelectSingleNode(Constants.ReviewDocTag.DOCCODE).InnerText);
                var nodeHeader = nodeRoot.SelectSingleNode(Constants.ReviewDocTag.HEADER);
                var nodeBody = nodeRoot.SelectSingleNode(Constants.ReviewDocTag.BODY);
                var nodeReviewStatus = nodeHeader.SelectSingleNode(Constants.ReviewDocTag.ReviewStatus);

                review.ReviewStatus = nodeReviewStatus.InnerText;
                var reviewMessage = new StringBuilder();

                if (reviewDocVersion < 200)
                {
                    var mostImportantMsg = GetMessage(nodeBody, Constants.ReviewDocTag.MostImportantMsgText, false);
                    var mostImportantMessage = new StringBuilder();
                    mostImportantMessage.Append(mostImportantMsg.ToString());

                    var devPriorities = GetMessage(nodeBody, Constants.ReviewDocTag.DevPrioritiesText, false);
                    if (!string.IsNullOrEmpty(devPriorities?.ToString()))
                    {
                        mostImportantMessage.Append(Constants.ReviewDocMessage.devPrioritiesHeading);
                        mostImportantMessage.Append(devPriorities);
                    }
                    CreateReviewMessage(mostImportantMessage, reviewMessage);

                }
                else
                {
                    var spcwMessage = GetMessage(nodeBody, Constants.ReviewDocTag.SPCW);
                    CreateReviewMessage(spcwMessage, reviewMessage, Constants.ReviewDocMessage.spcwHeading);

                    if (reviewDocVersion == 203 || reviewDocVersion == 210)
                    {
                        var cpdnMessage = GetMessage(nodeBody, Constants.ReviewDocTag.CPDN);
                        CreateReviewMessage(cpdnMessage, reviewMessage, Constants.ReviewDocMessage.cpdnHeading);
                    }
                    else if (reviewDocVersion == 204 || reviewDocVersion == 210)
                    {

                        var cdppMessage = GetMessage(nodeBody, Constants.ReviewDocTag.CDPP);
                        CreateReviewMessage(cdppMessage, reviewMessage, Constants.ReviewDocMessage.cdppHeading);
                    }
                }

                ConvertRichTextToHtml(review, reviewMessage);
            }
        }

        private static void ConvertRichTextToHtml(ReviewViewModel review, StringBuilder reviewMessage)
        {
            review.Document = RtfPipe.Rtf.ToHtml(reviewMessage.ToString());
        }
        private static void CreateReviewMessage(StringBuilder message, StringBuilder reviewMessage, string heading = null)
        {
            if (!string.IsNullOrEmpty(heading))
            {
                reviewMessage.Append(heading);
            }
            reviewMessage.Append(message.ToString());
        }

        private static StringBuilder GetMessage(XmlNode nodeBody, string docTag, bool noDataMessageRequired = true)
        {
            var message = new StringBuilder();
            var selectedNode = nodeBody.SelectSingleNode(docTag);

            if (!string.IsNullOrEmpty(selectedNode?.InnerText))
            {
                message.Append(selectedNode.InnerText.ToString());
            }
            else if (noDataMessageRequired)
            {
                message.Append(Constants.ReviewDocMessage.noDataAvailableMessage);
            }

            return message;
        }

        private static StringBuilder GetMessagePostCleanUp(XmlNode nodeBody, string docTag, string cleanupText)
        {
            var selectedNode = nodeBody.SelectSingleNode(docTag);
            var message = new StringBuilder();

            if (!string.IsNullOrEmpty(selectedNode?.InnerText))
            {
                message.Append(selectedNode.InnerText.ToString());
                var messageLength = message.Length;
                var stringSearhIndex = message.ToString().IndexOf(cleanupText);
                message.Remove(messageLength - 1, 1);
                message.Remove(0, stringSearhIndex - 1);

            }
            else
            {
                message.Append(" ");
            }

            return message;
        }

        private static XmlDocument LoadReviewData(ReviewViewModel review)
        {
            var xmlReview = new XmlDocument();
            xmlReview.LoadXml(review.Document);
            return xmlReview;
        }

        private IEnumerable<ReviewViewModel> ConvertToReviewViewModel(IEnumerable<Review> reviewsData)
        {
            var reviews = new List<ReviewViewModel>();
            foreach (var groupedReviewData in reviewsData.GroupBy(g => g.ReviewId).Select(grp => grp.ToList()))
            {
                var firstReviewFromGroup = groupedReviewData.FirstOrDefault();
                var review = new ReviewViewModel
                {
                    ReviewId = firstReviewFromGroup.ReviewId,
                    Document = firstReviewFromGroup.Document,
                    ReviewStatusCode = firstReviewFromGroup.ReviewStatusCode,
                    LastUpdated = firstReviewFromGroup.LastUpdated,
                    ReviewStatusDate = firstReviewFromGroup.ReviewStatusDate,
                    Ratings = new List<Rating>()
                };

                foreach (var reviewData in groupedReviewData)
                {
                    var rating = new Rating
                    {
                        RatingLabel = reviewData.FormItemTypeName,
                        RatingResult = reviewData.FormItemValue
                    };
                    review.Ratings.Add(rating);
                }

                reviews.Add(review);

            }

            return reviews;
        }
    }
}

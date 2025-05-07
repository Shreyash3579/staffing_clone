using Staffing.TableauAPI;
using Staffing.TableauAPI.FilesLogging;
using Staffing.TableauAPI.Helpers;
using Staffing.TableauAPI.Requests;
using System.Linq;
using System.Net;
using Xunit;

namespace Staffing.TableauAPI.Test
{
    public class SiteConnectionTest
    {
        [Fact]
        public void DownloadWorkbooksTest()
        {
            var url = new TableauServerUrls(ServerProtocol.Https, ConfigurationUtility.GetValue("TabCredentials:Server"),
                ConfigurationUtility.GetValue("TabCredentials:Site"), 10, ServerVersion.Server2020_3);

            var signIn = new TableauServerSignIn(
                url,
               ConfigurationUtility.GetValue("TabCredentials:UserId"),
               ConfigurationUtility.GetValue("TabCredentials:Password"), new TaskStatusLogs());
            signIn.ExecuteRequest();
            var a =
                new DownloadWorkbooksList(url, signIn);
            a.ExecuteRequest();
            Assert.Equal(2, a.Workbooks.Count);

            var b = new DownloadProjectsList(url, signIn);
            b.ExecuteRequest();
            Assert.Equal(1, b.Projects.Count());

            signIn = new TableauServerSignIn(
                url,
               ConfigurationUtility.GetValue("TabCredentials:UserId"),
               ConfigurationUtility.GetValue("TabCredentials:Password"), new TaskStatusLogs());
            signIn.ExecuteRequest();
            a =
                new DownloadWorkbooksList(
                    new TableauServerUrls(ServerProtocol.Http, "tableau.bain.com", "Staffing", 10, ServerVersion.Server9),
                    signIn);

            a.ExecuteRequest();
            Assert.Equal(4, a.Workbooks.Count);

            foreach (var workbook in a.Workbooks)
            {
                var viewQuery = new DownloadViewsForWorkbookList(workbook.Id, url, signIn);
                viewQuery.ExecuteRequest();
                Assert.Equal(1, viewQuery.Views.Count);
                foreach (var view in viewQuery.Views)
                {
                    var thumbnailQuery = new DownloadView(url, signIn);
                    var result = thumbnailQuery.GetPreviewImage(workbook.Id, view.Id);
                    Assert.NotEqual(0, result.Length);
                }
            }

            b = new DownloadProjectsList(url, signIn);
            b.ExecuteRequest();
            Assert.Equal(1, b.Projects.Count());

            var siteViews = new DownloadViewsForSiteList(url, signIn);
            siteViews.ExecuteRequest();
            Assert.Equal(0, siteViews.Views.Count);


        }

        [Fact]
        public void TicketTest()
        {
            var url = new TableauServerUrls(ServerProtocol.Https, ConfigurationUtility.GetValue("TabCredentials:Server"),
                ConfigurationUtility.GetValue("TabCredentials:Site"), 10, ServerVersion.Server2020_3);

            var signIn = new TableauServerSignIn(
                url,
               ConfigurationUtility.GetValue("TabCredentials:UserId"),
               ConfigurationUtility.GetValue("TabCredentials:Password"), new TaskStatusLogs());
            signIn.ExecuteRequest();

            var a = new TableauServerTicket(url, signIn);
            var ticket = a.Ticket();
            Assert.NotEqual("-1", ticket);
        }

        [Fact]
        public void ViewPdfLinkTest()
        {
            var url = new TableauServerUrls(ServerProtocol.Https, ConfigurationUtility.GetValue("TabCredentials:Server"),
               ConfigurationUtility.GetValue("TabCredentials:Site"), 10, ServerVersion.Server2020_3);

            var signIn = new TableauServerSignIn(
                url,
               ConfigurationUtility.GetValue("TabCredentials:UserId"),
               ConfigurationUtility.GetValue("TabCredentials:Password"), new TaskStatusLogs());
            signIn.ExecuteRequest();
            var workbooks =
                new DownloadWorkbooksList(
                    new TableauServerUrls(ServerProtocol.Http, ConfigurationUtility.GetValue("TabCredentials:Server"),
                    ConfigurationUtility.GetValue("TabCredentials:Site"), 10, ServerVersion.Server2020_3),
                    signIn);

            workbooks.ExecuteRequest();
            var workbook = workbooks.Workbooks.First();
            var viewQuery = new DownloadViewsForWorkbookList(workbook.Id, url, signIn);
            viewQuery.ExecuteRequest();
            var view = viewQuery.Views.First();
            var a = new TrustedUrls(workbook.Name.Replace(" ", ""), view.Name.Replace(" ", ""), url, signIn);
            var exportPdfUrl = a.GetExportPdfUrl();
            Assert.False(string.IsNullOrEmpty(exportPdfUrl));
            var thumbnailUrl = a.GetPreviewImageUrl();
            Assert.False(string.IsNullOrEmpty(thumbnailUrl));
            var viewUrl = a.GetTrustedViewUrl();
            Assert.False(string.IsNullOrEmpty(viewUrl));

            var client = new WebClient();

            var data = client.DownloadData(exportPdfUrl);
            Assert.NotNull(data);
            Assert.True(data.Any());

            data = null;

            var downloadView = new DownloadView(url, signIn);
            data = downloadView.GetPreviewImage(workbook.Id, view.Id);
            Assert.NotNull(data);
            Assert.True(data.Any());

            a.AddViewParameter("Gabba", "1");
            a.AddViewParameter("Hey", "2");

            exportPdfUrl = a.GetExportPdfUrl();
            Assert.True(exportPdfUrl.Contains("?"));
            Assert.True(exportPdfUrl.EndsWith("Gabba=1&Hey=2"));

            thumbnailUrl = a.GetPreviewImageUrl();
            Assert.True(thumbnailUrl.Contains("?"));
            Assert.True(thumbnailUrl.EndsWith("Gabba=1&Hey=2"));

            a.HideToolbar = true;
            viewUrl = a.GetTrustedViewUrl();
            Assert.True(viewUrl.EndsWith("&:toolbar=no"));

            a.HideToolbar = null;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("&:toolbar=no"));

            a.HideToolbar = false;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("&:toolbar=no"));

            a.HideTabs = true;
            viewUrl = a.GetTrustedViewUrl();
            Assert.True(viewUrl.EndsWith("&:tabs=no"));

            a.HideTabs = null;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("&:tabs=no"));

            a.HideTabs = false;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("&:tabs=no"));

            a = new TrustedUrls(workbook.Name.Replace(" ", ""), view.Name.Replace(" ", ""), url, signIn);
            a.HideToolbar = true;
            viewUrl = a.GetTrustedViewUrl();
            Assert.True(viewUrl.EndsWith("?:toolbar=no"));

            a.HideToolbar = null;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("?:toolbar=no"));

            a.HideToolbar = false;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("?:toolbar=no"));

            a.HideTabs = true;
            viewUrl = a.GetTrustedViewUrl();
            Assert.True(viewUrl.EndsWith("?:tabs=no"));

            a.HideTabs = null;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("?:tabs=no"));

            a.HideTabs = false;
            viewUrl = a.GetTrustedViewUrl();
            Assert.False(viewUrl.EndsWith("?:tabs=no"));

            a.HideToolbar = true;
            a.HideTabs = true;
            viewUrl = a.GetTrustedViewUrl();
            Assert.True(viewUrl.EndsWith("?:tabs=no&:toolbar=no"));
        }
    }
}

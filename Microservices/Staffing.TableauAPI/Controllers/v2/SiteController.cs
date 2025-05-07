using System;
using System.Linq;
using AutoMapper;
using Staffing.TableauAPI.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Staffing.TableauAPI.Repositories;
using System.Collections.Generic;
using Staffing.TableauAPI.Entities;
using Staffing.TableauAPI.Models;
using Staffing.TableauAPI.Helpers;
using System.Text.Json;

namespace Staffing.TableauAPI.v2.Controllers
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly ISiteRepository _SiteRepository;
        private readonly IUrlHelper _urlHelper;
        private readonly IMapper _mapper;

        public SiteController(
            IUrlHelper urlHelper,
            ISiteRepository SiteRepository,
            IMapper mapper)
        {
            _SiteRepository = SiteRepository;
            _mapper = mapper;
            _urlHelper = urlHelper;
        }

        [HttpGet(Name = nameof(GetAllSites))]
        public ActionResult GetAllSites(ApiVersion version, [FromQuery] QueryParameters queryParameters)
        {
            List<SiteEntity> SiteItems = _SiteRepository.GetAll(queryParameters).ToList();

            var allItemCount = _SiteRepository.Count();

            var paginationMetadata = new
            {
                totalCount = allItemCount,
                pageSize = queryParameters.PageCount,
                currentPage = queryParameters.Page,
                totalPages = queryParameters.GetTotalPages(allItemCount)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var links = CreateLinksForCollection(queryParameters, allItemCount, version);

            var toReturn = SiteItems.Select(x => ExpandSingleSiteItem(x, version));

            return Ok(new
            {
                value = toReturn,
                links = links
            });
        }




        [HttpGet]
        [Route("{id:int}", Name = nameof(GetSingleSite))]
        public ActionResult GetSingleSite(ApiVersion version, int id)
        {
            SiteEntity SiteItem = _SiteRepository.GetSingle(id);

            if (SiteItem == null)
            {
                return NotFound();
            }

            return Ok(ExpandSingleSiteItem(SiteItem, version));
        }

        [HttpPost(Name = nameof(AddSite))]
        public ActionResult<SiteDto> AddSite(ApiVersion version, [FromBody] SiteCreateDto SiteCreateDto)
        {
            if (SiteCreateDto == null)
            {
                return BadRequest();
            }

            SiteEntity toAdd = _mapper.Map<SiteEntity>(SiteCreateDto);

            _SiteRepository.Add(toAdd);

            if (!_SiteRepository.Save())
            {
                throw new Exception("Creating a Siteitem failed on save.");
            }

            SiteEntity newSiteItem = _SiteRepository.GetSingle(toAdd.Id);

            return CreatedAtRoute(nameof(GetSingleSite),
                new { version = version.ToString(), id = newSiteItem.Id },
                _mapper.Map<SiteDto>(newSiteItem));
        }

        [HttpPatch("{id:int}", Name = nameof(PartiallyUpdateSite))]
        public ActionResult<SiteDto> PartiallyUpdateSite(int id, [FromBody] JsonPatchDocument<SiteUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            SiteEntity existingEntity = _SiteRepository.GetSingle(id);

            if (existingEntity == null)
            {
                return NotFound();
            }

            SiteUpdateDto SiteUpdateDto = _mapper.Map<SiteUpdateDto>(existingEntity);
            patchDoc.ApplyTo(SiteUpdateDto);

            TryValidateModel(SiteUpdateDto);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(SiteUpdateDto, existingEntity);
            SiteEntity updated = _SiteRepository.Update(id, existingEntity);

            if (!_SiteRepository.Save())
            {
                throw new Exception("Updating a Siteitem failed on save.");
            }

            return Ok(_mapper.Map<SiteDto>(updated));
        }

        [HttpDelete]
        [Route("{id:int}", Name = nameof(RemoveSite))]
        public ActionResult RemoveSite(int id)
        {
            SiteEntity SiteItem = _SiteRepository.GetSingle(id);

            if (SiteItem == null)
            {
                return NotFound();
            }

            _SiteRepository.Delete(id);

            if (!_SiteRepository.Save())
            {
                throw new Exception("Deleting a Siteitem failed on save.");
            }

            return NoContent();
        }

        [HttpPut]
        [Route("{id:int}", Name = nameof(UpdateSite))]
        public ActionResult<SiteDto> UpdateSite(int id, [FromBody] SiteUpdateDto SiteUpdateDto)
        {
            if (SiteUpdateDto == null)
            {
                return BadRequest();
            }

            var existingSiteItem = _SiteRepository.GetSingle(id);

            if (existingSiteItem == null)
            {
                return NotFound();
            }

            _mapper.Map(SiteUpdateDto, existingSiteItem);

            _SiteRepository.Update(id, existingSiteItem);

            if (!_SiteRepository.Save())
            {
                throw new Exception("Updating a Siteitem failed on save.");
            }

            return Ok(_mapper.Map<SiteDto>(existingSiteItem));
        }

        [HttpGet("GetRandomSite", Name = nameof(GetRandomSite))]
        public ActionResult GetRandomSite()
        {
            ICollection<SiteEntity> SiteItems = _SiteRepository.GetRandomSite();

            IEnumerable<SiteDto> dtos = SiteItems
                .Select(x => _mapper.Map<SiteDto>(x));

            var links = new List<LinkDto>();

            // self 
            links.Add(new LinkDto(_urlHelper.Link(nameof(GetRandomSite), null), "self", "GET"));

            return Ok(new
            {
                value = dtos,
                links = links
            });
        }

        private List<LinkDto> CreateLinksForCollection(QueryParameters queryParameters, int totalCount, ApiVersion version)
        {
            var links = new List<LinkDto>();

            // self 
            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllSites), new
            {
                pagecount = queryParameters.PageCount,
                page = queryParameters.Page,
                orderby = queryParameters.OrderBy
            }), "self", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllSites), new
            {
                pagecount = queryParameters.PageCount,
                page = 1,
                orderby = queryParameters.OrderBy
            }), "first", "GET"));

            links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllSites), new
            {
                pagecount = queryParameters.PageCount,
                page = queryParameters.GetTotalPages(totalCount),
                orderby = queryParameters.OrderBy
            }), "last", "GET"));

            if (queryParameters.HasNext(totalCount))
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllSites), new
                {
                    pagecount = queryParameters.PageCount,
                    page = queryParameters.Page + 1,
                    orderby = queryParameters.OrderBy
                }), "next", "GET"));
            }

            if (queryParameters.HasPrevious())
            {
                links.Add(new LinkDto(_urlHelper.Link(nameof(GetAllSites), new
                {
                    pagecount = queryParameters.PageCount,
                    page = queryParameters.Page - 1,
                    orderby = queryParameters.OrderBy
                }), "previous", "GET"));
            }

            var posturl = _urlHelper.Link(nameof(AddSite), new { version = version.ToString() });

            links.Add(
               new LinkDto(posturl,
               "create_Site",
               "POST"));

            return links;
        }

        private dynamic ExpandSingleSiteItem(SiteEntity SiteItem, ApiVersion version)
        {
            var links = GetLinks(SiteItem.Id, version);
            SiteDto item = _mapper.Map<SiteDto>(SiteItem);

            var resourceToReturn = item.ToDynamic() as IDictionary<string, object>;
            resourceToReturn.Add("links", links);

            return resourceToReturn;
        }

        private IEnumerable<LinkDto> GetLinks(int id, ApiVersion version)
        {
            var links = new List<LinkDto>();

            var getLink = _urlHelper.Link(nameof(GetSingleSite), new { version = version.ToString(), id = id });

            links.Add(
              new LinkDto(getLink, "self", "GET"));

            var deleteLink = _urlHelper.Link(nameof(RemoveSite), new { version = version.ToString(), id = id });

            links.Add(
              new LinkDto(deleteLink,
              "delete_Site",
              "DELETE"));

            var createLink = _urlHelper.Link(nameof(AddSite), new { version = version.ToString() });

            links.Add(
              new LinkDto(createLink,
              "create_Site",
              "POST"));

            var updateLink = _urlHelper.Link(nameof(UpdateSite), new { version = version.ToString(), id = id });

            links.Add(
               new LinkDto(updateLink,
               "update_Site",
               "PUT"));

            return links;
        }

    }
}

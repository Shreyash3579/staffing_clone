using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SecurityUserController : ControllerBase
    {
        private readonly ISecurityUserService _securityUserService;
        public SecurityUserController(ISecurityUserService securityUserService)
        {
            _securityUserService = securityUserService;
        }
        /// <summary>
        /// Get all security / staffing users
        /// </summary>
        /// <returns></returns>
        [HttpGet("getAllSecurityUsers")]
        [Authorize]
        public async Task<IActionResult> GetAllSecurityUsers()
        {
            var securityUsers = await _securityUserService.GetAllSecurityUsers();
            return Ok(securityUsers);
        }

        /// <summary>
        /// Delete security user by employee code
        /// </summary>
        ///<param name="employeeCode">employeeCode of the user about to delete</param>
        /// <param name="lastUpdatedBy">employeeCode of the user who triggered the delete</param>
        /// <returns></returns>
        [HttpDelete("deleteSecurityUser")]
        [Authorize]
        public async Task<IActionResult> DeleteSecurityUserByEmployeeCode(string employeeCode, string lastUpdatedBy)
        {
            await _securityUserService.DeleteSecurityUserByEmployeeCode(employeeCode,lastUpdatedBy);
            return Ok();
        }

        /// <summary>
        /// Update security user with admin rights
        /// </summary>
        /// <param name="securityUser"></param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "employeeCode":"55555",
        ///       "lastUpdatedBy":"51030",
        ///       "isAdmin": false
        ///    }
        /// </remarks>
        /// <returns></returns>
        [HttpPost("upsertSecurityUser")]
        [Authorize]
        public async Task<IActionResult> UpsertSecurityUser(SecurityUser securityUser)
        {
            var updatedSecurityUser = await _securityUserService.UpsertBOSSSecurityUser(securityUser);
            return Ok(updatedSecurityUser);
        }

        /// <summary>
        /// Get all security / staffing groups
        /// </summary>
        /// <returns>List of security group names and roles they have access to</returns>
        [HttpGet("getAllSecurityGroups")]
        [Authorize]
        public async Task<IActionResult> GetAllSecurityGroups()
        {
            var securityUsers = await _securityUserService.GetAllSecurityGroups();
            return Ok(securityUsers);
        }

        /// <summary>
        /// Get group names by search string
        /// </summary>
        /// <returns>List of group names by their names</returns>
        [HttpGet("getGroupNamesBySearchString")]
        [Authorize]
        public async Task<IActionResult> GetGroupNamesBySearchString(string searchString)
        {
            var groupNames = await _securityUserService.GetGroupNamesBySearchString(searchString);
            return Ok(groupNames);
        }

        /// <summary>
        /// Update security group
        /// </summary>
        /// <param name="securityGroup"></param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "groupName":"Global Staffing Development Team",
        ///       "lastUpdatedBy":"39209",
        ///       "roleCodes": "1,12"
        ///    }
        /// </remarks>
        /// <returns>Upserted Data</returns>
        [HttpPost("upsertSecurityGroup")]
        [Authorize]
        public async Task<IActionResult> UpsertSecurityGroup(SecurityGroup securityGroup)
        {
            var updatedSecurityGroup = await _securityUserService.UpsertBOSSSecurityGroup(securityGroup);
            return Ok(updatedSecurityGroup);
        }

        /// <summary>
        /// Delete security user by employee code
        /// </summary>
        ///<param name="groupIdToDelete">database table id of the group to delete</param>
        /// <param name="lastUpdatedBy">employeeCode of the user who triggered the delete</param>
        /// <returns></returns>
        [HttpDelete("deleteSecurityGroup")]
        [Authorize]
        public async Task<IActionResult> DeleteSecurityGroupById(string groupIdToDelete, string lastUpdatedBy)
        {
            await _securityUserService.DeleteSecurityGroupById(groupIdToDelete, lastUpdatedBy);
            return Ok();
        }


        [HttpGet("getRevOfficeList")]
        public async Task<IActionResult> GetOfficeRevList()
        {
            var officeList = await _securityUserService.GetRevOfficeList();
            return Ok(officeList);
        }

        [HttpGet("getServiceLineList")]
        public async Task<IActionResult> GetServiceLineList()
        {
            var serviceLineList = await _securityUserService.GetServiceLineList();
            return Ok(serviceLineList);
        }

        [HttpPost("saveServiceLineList")]
        public async Task<IActionResult> SaveServiceLineList(IEnumerable<ServiceLineHierarchy> serviceLineList)
        {
            await _securityUserService.SaveServiceLineList(serviceLineList);
            return Ok();
        }

        [HttpPost("saveRevOfficeList")]
        public async Task<IActionResult> saveRevOfficeList(IEnumerable<RevOffice> officeList)
        {
            await _securityUserService.saveRevOfficeList(officeList);
            return Ok();
        }

        [HttpPost("updateSecurityUserForWFPRole")]
        public async Task<IActionResult> UpdateSecurityUpdateSecurityUserForWFPRoleUserForWFPUsers(OfficeServiceLineHierarchy securityUserUpdate)
        {
            await _securityUserService.UpdateSecurityUserForWFPRole(securityUserUpdate.NewOffices, securityUserUpdate.NewServiceLines);
            return Ok();
        }


    }
}

﻿using Abp.ObjectMapping;
using IEManageSystem.Web.Controllers;
using IEManageSystem.ApiAuthorization.DomainModel;
using IEManageSystem.ApiScopeProviders;
using IEManageSystem.CMS.DomainModel.PageDatas;
using IEManageSystem.CMS.DomainModel.Pages;
using IEManageSystem.Entitys.Authorization.Permissions;
using IEManageSystem.JwtAuthentication;
using IEManageSystem.Services.ManageHome.CMS.Pages;
using IEManageSystem.Services.ManageHome.CMS.Pages.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IEManageSystem.Web.Controllers.CMS.Pages.Dto;
using IEManageSystem.Attributes;

namespace IEManageSystem.Web.Controllers.CMS.Pages
{
    [Route("api/[controller]/[action]")]
    public class PageDataManageController : IEManageSystemControllerBase
    {
        private IPageDataManageAppService _pageDataManageAppService { get; }

        private PageManager _pageManager { get; }

        private PageDataManager _pageDataManager { get; set; }

        private PermissionManager _permissionManager { get; }

        private ClaimManager _claimManager { get; }

        private CheckPermissionService _checkPermissionService { get; }

        public PageDataManageController(
            IPageDataManageAppService pageDataManageAppService,
            PageManager pageManager,
            PermissionManager permissionManager,
            ClaimManager claimManager,
            CheckPermissionService checkPermissionService,
             PageDataManager pageDataManager
            )
        {
            _pageDataManageAppService = pageDataManageAppService;
            _pageManager = pageManager;
            _permissionManager = permissionManager;
            _claimManager = claimManager;
            _checkPermissionService = checkPermissionService;
            _pageDataManager = pageDataManager;
        }

        /// <summary>
        /// 是否拥有管理权限
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private bool IsCanManage(string pageName) 
        {
            IEnumerable<string> permissionNames = _claimManager.GetPermissionsForClaims(User.Claims);
            var permissions = _permissionManager.GetPermissionsForCache().Where(e => permissionNames.Contains(e.Name));

            return _pageManager.IsCanManagePost(pageName, permissions) || 
                _checkPermissionService.IsAllowAccess(ApiScopeProvider.Page, false, permissions);
        }

        /// <summary>
        /// 是否拥有查询权限
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        private bool IsQueryAccess(string pageName)
        {
            IEnumerable<string> permissionNames = _claimManager.GetPermissionsForClaims(User.Claims);
            var permissions = _permissionManager.GetPermissionsForCache().Where(e => permissionNames.Contains(e.Name));

            return _pageManager.IsCanQueryPost(pageName, permissions) ||
                _checkPermissionService.IsAllowAccess(ApiScopeProvider.Page, false, permissions);
        }

        [HttpPost]
        public ActionResult<AddPageDataOutput> AddPageData([FromBody] AddPageDataInput input)
        {
            if (!IsCanManage(input.PageName))
            {
                throw new Abp.Authorization.AbpAuthorizationException("未授权操作");
            }

            return _pageDataManageAppService.AddPageData(input);
        }

        [HttpPost]
        public ActionResult<UpdatePageDataOutput> UpdatePageData([FromBody] UpdatePageDataInput input)
        {
            if (!IsCanManage(input.PageName))
            {
                throw new Abp.Authorization.AbpAuthorizationException("未授权操作");
            }

            return _pageDataManageAppService.UpdatePageData(input);
        }

        [HttpPost]
        public ActionResult<DeletePageDataOutput> DeletePageData([FromBody] DeletePageDataInput input)
        {
            if (!IsCanManage(input.PageName))
            {
                throw new Abp.Authorization.AbpAuthorizationException("未授权操作");
            }

            return _pageDataManageAppService.DeletePageData(input);
        }

        [HttpPost]
        public ActionResult<UpdateComponentDataOutput> UpdateComponentData([FromBody] UpdateComponentDataInput input)
        {
            if (!IsCanManage(input.PageName))
            {
                throw new Abp.Authorization.AbpAuthorizationException("未授权操作");
            }

            return _pageDataManageAppService.UpdateComponentData(input);
        }

        /// <summary>
        /// 文章评分
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiAuthorization]
        public ActionResult<ScorePageDataOutput> ScorePageData([FromBody] ScorePageDataInput input) 
        {
            int pageId;
            if (int.TryParse(input.PageName, out pageId))
            {
                string pageName = _pageManager.GetPageNameCache(pageId);

                if (!string.IsNullOrWhiteSpace(pageName))
                {
                    input.PageName = pageName;
                }
            }

            if (!IsQueryAccess(input.PageName))
            {
                throw new Abp.Authorization.AbpAuthorizationException("未授权操作");
            }

            var userInfo = _claimManager.CreateUserClaimInfo(User.Claims);
            int userId;
            int.TryParse(userInfo.Subject, out userId);
            var post = _pageDataManager.PostRepository.FirstOrDefault(e => e.Name == input.PageDataName && e.Page.Name == input.PageName);
            post.ToScore(input.Score, userId);

            return new ScorePageDataOutput();
        }
    }
}

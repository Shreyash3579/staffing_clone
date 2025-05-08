import { NgModule } from "@angular/core";
import { Routes, RouterModule, PreloadingStrategy, PreloadAllModules } from "@angular/router";
import { AuthGuardService, canMatchApplicationRoutes } from "./core/authentication/auth-guard.service";
import { AccessDeniedComponent } from "./core/access-denied/access-denied.component";
import { SiteMaintainanceComponent } from "./core/site-maintainance/site-maintainance.component";

type PathMatch = "full" | "prefix" | undefined; //support Angular 14 upgrade 

const routes: Routes = [
  //staffing 2.0 is default hence home route is now loadng homecopy.. Will clean-up once staffing tab is retired
  { path: "", redirectTo: "/home", pathMatch: "full" as PathMatch },
  { 
    path: "homeCopy",
    loadChildren: () => import("./home/home.module").then((m) => m.HomeModule),
    // canMatch: [canMatchApplicationRoutes],
    canLoad: [AuthGuardService]
  },
  {
    path: "home",
    loadChildren: () => import("./home-copy/home.module").then((m) => m.HomeModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "casePlanning",
    loadChildren: () => import("./case-planning/case-planning.module").then((m) => m.CasePlanningModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "casePlanningCopy",
    loadChildren: () => import("./case-planning-copy/case-planning.module").then((m) => m.CasePlanningModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "resources",
    loadChildren: () => import("./resources/resources.module").then((m) => m.ResourcesModule),
    canLoad: [AuthGuardService],
    // canMatch: [canMatchApplicationRoutes]
  },
  {
    path: "analytics",
    loadChildren: () => import("./analytics/analytics.module").then((m) => m.AnalyticsModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "admin",
    loadChildren: () => import("./admin/admin.module").then((m) => m.AdminModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "overlay",
    loadChildren: () => import("./overlay/overlay.module").then((m) => m.OverlayModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "employee",
    loadChildren: () =>
      import("./staffing-resource-popup/staffing-resource-popup.module").then((m) => m.StaffingResourcePopupModule),
    // canMatch: [canMatchApplicationRoutes]
    canLoad: [AuthGuardService]
  },
  {
    path: "staffingInsightsTool",
    loadComponent: () =>
      import("./staffing-insights-tool/staffing-insights-tool-container/staffing-insights-tool-container.component").then((m) => m.StaffingInsightsToolContainerComponent),
    canMatch: [canMatchApplicationRoutes],
  },
  {
    path: 'intakeForm',
    loadChildren: () => import('./staffing-intake-form/staffing-intake-form.module').then((m)=> m.StaffingIntakeFormModule),
    canLoad: [AuthGuardService]
  },
  { path: "accessdenied", component: AccessDeniedComponent },
  { path: "offline", component: SiteMaintainanceComponent },
  { path: "**", redirectTo: "home" }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { preloadingStrategy: PreloadAllModules})],
  exports: [RouterModule]
})
export class AppRoutingModule {}

import { Directive, Input, TemplateRef, ViewContainerRef } from '@angular/core';
import { CoreService } from 'src/app/core/core.service';
import { CommonService } from 'src/app/shared/commonService';

@Directive({
  selector: '[appFeaturesAccess]'
})
//This directive is used to show or hide an HTML element depending on user's access.
export class FeaturesAccessDirective {

  private hasView = false;

  constructor(
    private templateRef: TemplateRef<any>,
    private viewContainer: ViewContainerRef,
    private coreService: CoreService) { }

  @Input() set appFeaturesAccess(featureName: string) {
    const accessibleFeatures = this.coreService.loggedInUserClaims?.FeatureAccess.map(x => x.FeatureName);
    const hasAccess = CommonService.hasAccessToFeature(featureName, accessibleFeatures);
    
    if (hasAccess && !this.hasView) {
      this.viewContainer.createEmbeddedView(this.templateRef);
      this.hasView = true;
    } else if (!hasAccess && this.hasView) {
      this.viewContainer.clear();
      this.hasView = false;
    }
  }
}

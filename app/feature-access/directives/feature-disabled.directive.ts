import { Directive, ElementRef, HostListener, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CoreService } from 'src/app/core/core.service';
import { CommonService } from 'src/app/shared/commonService';

@Directive({
  selector: '[appFeatureDisabled]'
})
export class FeatureDisabledDirective {

  constructor(private elementRef: ElementRef, private coreService: CoreService) { }

  @Input() set claimBasedFeatureDisabled(featureName: string) {
    const accessibleFeatures = this.coreService.loggedInUserClaims.FeatureAccess;
    const isReadOnly = CommonService.isReadOnlyAccessToFeature(featureName, accessibleFeatures);
    const isLinkDisabled = CommonService.isLinkDisabledForFeature(featureName, accessibleFeatures);

    if (isReadOnly || isLinkDisabled) {
      this.elementRef.nativeElement.classList.add('read-only');
    } else {
      this.elementRef.nativeElement.classList.remove('read-only');
    }
  }

  @Input() set isDisabled(isDisabled: boolean) {
    if(isDisabled){
        this.elementRef.nativeElement.classList.add('read-only');
    }
  }
}

import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FeaturesAccessDirective } from './directives/features-access.directive';
import { FeatureDisabledDirective } from './directives/feature-disabled.directive';



@NgModule({
  declarations: [FeaturesAccessDirective, FeatureDisabledDirective],
  imports: [
    CommonModule
  ],
  exports:[FeaturesAccessDirective, FeatureDisabledDirective]
})
export class FeatureAccessModule { }

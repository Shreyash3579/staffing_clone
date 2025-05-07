import { NgModule } from '@angular/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TypeaheadModule } from 'ngx-bootstrap/typeahead';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';

@NgModule({
  imports: [
    ModalModule.forRoot(),
    TypeaheadModule.forRoot(),
    BsDatepickerModule.forRoot(),
    BsDropdownModule.forRoot()
  ],
  exports: [
    ModalModule,
    TypeaheadModule,
    BsDatepickerModule,
    BsDropdownModule
  ]
})
export class AppBootstrapModule {}

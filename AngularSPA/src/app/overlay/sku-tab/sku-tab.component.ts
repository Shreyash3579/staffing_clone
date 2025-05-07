import { Component, OnInit, Input, Output, EventEmitter, OnChanges, SimpleChanges, ViewChild, ElementRef } from '@angular/core';
import { ValidationService } from 'src/app/shared/validationService';
import { BsDatepickerConfig, BsDatepickerDirective } from 'ngx-bootstrap/datepicker';
import { SkuTerm } from 'src/app/shared/interfaces/skuTerm.interface';
import { DateService } from 'src/app/shared/dateService';

@Component({
  selector: 'app-sku-tab',
  templateUrl: './sku-tab.component.html',
  styleUrls: ['./sku-tab.component.scss']
})
export class SkuTabComponent implements OnInit, OnChanges {
  @Input() skuTab: any;
  @Input() skuTermList: SkuTerm[];
  @Output() deleteSkuTab = new EventEmitter<any>();
  @Output() updateSkuCaseTerms = new EventEmitter<any>();

  @ViewChild(BsDatepickerDirective, {static: false}) datepicker: BsDatepickerDirective;
  @ViewChild('skuDateEl', {static: false}) skuDateElement: ElementRef;

  termEditing = false;
  tabEditing = false;
  asyncSkuString: string;
  skuCaseTerms = [];
  skuEffectiveDate: Date;
  bsConfig: Partial<BsDatepickerConfig>;
  validationObj = { isValid : true , errorMessage: ''};

  constructor() { }

  ngOnInit() {

    this.bsConfig = {
      containerClass: 'theme-red',
      customTodayClass: 'custom-today-class',
      dateInputFormat: 'DD-MMM-YYYY',
      isAnimated : true,
      showWeekNumbers: false,
        selectFromOtherMonth : true
    }

  }

  ngOnChanges(simpleChanges: SimpleChanges) {

    if (simpleChanges.skuTab) {

      this.skuEffectiveDate = new Date(this.skuTab.effectiveDate);

      this.setSkuCaseTerms();

    }

  }

  setSkuCaseTerms(){

    if (this.skuTermList && this.skuTab.skuTermsCodes) {
      this.skuCaseTerms = this.skuTermList.filter(term => {
        return this.skuTab.skuTermsCodes.split(',').indexOf(term.code.toString()) > -1 ;
      });
    }

    // If there are no sku for case/opportunity, show empty tab with input text box for entering new terms
    if (!this.skuCaseTerms.length) {
      this.termEditing = true;
    }

  }

  //-------------------------------Event Handlers--------------------------------------//

  enableSkuTermEdit() {
    this.termEditing  = true;
  }

  addSkuTerm(skuTerm) {

    this.skuCaseTerms.push(skuTerm.item);
    this.asyncSkuString = '';
    this.termEditing = false;
    this.skuTab.skuTermsCodes = this.skuCaseTerms.map(caseTerm => {
      return caseTerm.code;
    }).join(',');

    let skuData = {
      skuTab: this.skuTab,
      skuTerms: this.skuCaseTerms
    }

    this.updateSkuCaseTerms.emit(skuData);

  }

  deleteSkuTerm(skuTerm) {

    this.skuCaseTerms.splice(this.skuCaseTerms.indexOf(skuTerm), 1);

    this.skuTab.skuTermsCodes = this.skuCaseTerms.map(caseTerm => {
      return caseTerm.code;
    }).join(',');

    let skuData = {
      skuTab: this.skuTab,
      skuTerms: this.skuCaseTerms
    }

    this.updateSkuCaseTerms.emit(skuData);

  }

  enableTabDateEdit(event) {

    this.tabEditing = true;

    setTimeout(() => {
      this.datepicker.show();
      this.skuDateElement.nativeElement.select();
      this.skuDateElement.nativeElement.focus();
    });

  }

  disableTabDateEdit(event){

    const _self = this;

    setTimeout(() => {

      if(event.relatedTarget && event.relatedTarget.type == "button" || !this.validationObj.isValid){
        event.preventDefault();
        return false;
      }else{
        _self.tabEditing = false;
        _self.datepicker.hide();
      }

    }, 200); //DO NOT decrease the time. We need datepicker change to fire first and then disable to occur

  }

  onSkuDateChange(changedDate){
    this.validationObj = ValidationService.validateDate(changedDate);

    if(this.validationObj.isValid){

      this.skuTab.effectiveDate = DateService.convertDateInBainFormat(changedDate);;
      this.disableTabDateEdit(""); //This is done to close editing of input as it remained open when you changed date afer correcting an error in input

      this.updateSkuCaseTerms.emit(this.skuTab);

    }else{

      this.skuTab.effectiveDate = '';

    }
  }

  deleteSkuTabHandler() {
    this.deleteSkuTab.emit(this.skuTab);
  }

}

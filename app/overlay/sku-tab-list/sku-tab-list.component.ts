import {
  Component, OnInit, DoCheck, ChangeDetectionStrategy, Input, Output,
  EventEmitter, OnChanges, ViewChild
} from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { TabsetComponent } from 'ngx-bootstrap/tabs';

@Component({
  selector: 'app-sku-tab-list',
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sku-tab-list.component.html',
  styleUrls: ['./sku-tab-list.component.scss']
})
export class SkuTabListComponent implements OnInit, OnChanges {
  isAddNewButtonDisabled = true;

  @Input() oldCaseCode: string;
  @Input() pipelineId: string;
  @Input() skuTabList: any;
  @Input() skuTermList: any;
  @Input() caseStartDate: any;
  @Input() estimatedTeamSize: any;
  @Input() pricingTeamSize: any;

  @Output() insertSkuCaseTerms = new EventEmitter<any>();
  @Output() updateSkuCaseTerms = new EventEmitter<any>();
  @Output() deleteSkuCaseTerms = new EventEmitter<any>();

  @ViewChild(TabsetComponent, { static: true }) skuTabs: TabsetComponent;

  constructor() { }

  ngOnInit() {
  }

  ngOnChanges(simpleChanges) {
    if (simpleChanges.skuTabList && simpleChanges.skuTabList.currentValue && simpleChanges.skuTabList.currentValue.length === 0) {
      this.isAddNewButtonDisabled = true;
      this.addSkuTab();
    } else {
      this.isAddNewButtonDisabled = false;
    }
  }


  addSkuTab() {
    const skuTab = {
      id: null,
      oldCaseCode: this.oldCaseCode,
      pipelineId: this.pipelineId,
      effectiveDate: (this.caseStartDate != null && <any>new Date(this.caseStartDate) > DateService.getToday())
        ? this.caseStartDate : DateService.getBainFormattedToday(),
      skuTermsCodes: this.skuTabList.length ? this.skuTabList[this.skuTabList.length - 1].skuTermsCodes : ''
    };

    this.skuTabList.push(skuTab);


  }

  deleteSkuTab(skuTab) {
    this.skuTabList.splice(this.skuTabList.indexOf(skuTab), 1);

    if (skuTab.id) {
      this.deleteSkuCaseTerms.emit(skuTab);
    }

    if (this.skuTabList.length === 0) {
      this.isAddNewButtonDisabled = false;
    }
  }

  updateSkuCaseTermsHandler(skuData) {
    if (!skuData.skuTab.id) {
      this.insertSkuCaseTerms.emit(skuData);
    } else {
      this.updateSkuCaseTerms.emit(skuData);
    }
  }

}

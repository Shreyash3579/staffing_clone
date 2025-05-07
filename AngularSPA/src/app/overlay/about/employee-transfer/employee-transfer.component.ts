import { Component, Input,OnChanges,OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { DateService } from 'src/app/shared/dateService';
import { TransferTransactionModel } from 'src/app/shared/interfaces/tranfer-transaction.interface';

@Component({
  selector: 'app-employee-transfer',
  templateUrl: './employee-transfer.component.html',
  styleUrls: ['./employee-transfer.component.scss']
})
export class EmployeeTransferComponent implements OnInit, OnChanges {

  @Input() tranferData: TransferTransactionModel[];
  data: any;
 
  constructor() { }

  ngOnInit(): void {
  
  }

  ngOnChanges(changes: SimpleChanges): void {
    if(changes.tranferData && this.tranferData){
      this.tranferData = this.sortDateDesc(this.tranferData);
    }
  }

  getFormattedDate(date: string) {
    if (date == null) {
      return null;
    }
    return DateService.convertDateInBainFormat(date);
  }

  sortDateDesc(data){
    return data.sort((a, b) => new Date(b.startDate).getTime() - new Date(a.startDate).getTime());
  }

}

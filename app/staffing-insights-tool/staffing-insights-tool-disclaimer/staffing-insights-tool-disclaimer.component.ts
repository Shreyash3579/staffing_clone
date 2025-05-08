import { Component, EventEmitter, Output } from '@angular/core';

@Component({
  selector: 'app-staffing-insights-tool-disclaimer',
  standalone: true,
  imports: [],
  templateUrl: './staffing-insights-tool-disclaimer.component.html',
  styleUrl: './staffing-insights-tool-disclaimer.component.scss'
})
export class StaffingInsightsToolDisclaimerComponent {
  @Output() disclaimerReadEmitter = new EventEmitter<boolean>();

  setDisclaimerRead(){
    this.disclaimerReadEmitter.emit(true);
  }
}

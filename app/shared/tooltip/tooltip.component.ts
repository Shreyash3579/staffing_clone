import { Component, Input, OnInit, ElementRef } from "@angular/core";

@Component({
  selector: "app-tooltip",
  templateUrl: "./tooltip.component.html",
  styleUrls: ["./tooltip.component.scss"]
})
export class TooltipComponent implements OnInit {
  @Input() message: string;
  @Input() header: string;
  @Input() width: string;

  constructor(private elementRef: ElementRef) {}

  ngOnInit(): void {
    const tooltipWrapper = this.elementRef.nativeElement.querySelector(".tooltip-component");

    if (this.width) {
      tooltipWrapper.style.width = this.width + "px";
      tooltipWrapper.style.height = this.width + "px";
    }
  }
}

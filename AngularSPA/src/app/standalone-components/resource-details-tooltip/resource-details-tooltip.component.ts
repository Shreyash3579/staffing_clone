import {Component, Input} from "@angular/core";
import {NgIf} from "@angular/common";
interface TooltipData {
  resource: { name: string, office: string, type: string };
  startDate: string;
  endDate: string;
  resourceName: string;
  resourceLocation: string;
  experience: number;
  extraLang: string;
  info: string;
  alertMessage: string;
}

@Component({
  selector: 'app-resource-details-tooltip',
  standalone: true,
  imports: [
    NgIf
  ],
  templateUrl: './resource-details-tooltip.component.html',
  styleUrl: './resource-details-tooltip.component.scss'
})
export class ResourceDetailsTooltipComponent {
  @Input() data: TooltipData;
}

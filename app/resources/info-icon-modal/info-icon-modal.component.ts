import { Component, OnInit, Input } from "@angular/core";
import { BsModalRef } from "ngx-bootstrap/modal";

@Component({
    selector: "app-info-modal",
    templateUrl: "./info-icon-modal.component.html",
    styleUrls: ["./info-icon-modal.component.scss"]
})
export class InfoIconModalComponent implements OnInit {

    constructor(public modalRef: BsModalRef) {}

    ngOnInit(): void {}

    closeForm() {
        this.modalRef.hide();
      }
}

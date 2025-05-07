import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { BsDatepickerConfig, BsDatepickerDirective } from "ngx-bootstrap/datepicker";
import { BsModalRef } from "ngx-bootstrap/modal";
import { BS_DEFAULT_CONFIG } from "../constants/bsDatePickerConfig";
import { DateService } from "../dateService";
import { ValidationService } from "../validationService";


@Component({
    selector: 'app-inline-editable-date',
    templateUrl: './inline-editable-date-component.html',
    styleUrls: ['./inline-editable-date-component.scss']
})
export class InlineEditableDateComponent implements OnInit {
    public bsModalRef: BsModalRef;
    public bsConfig: Partial<BsDatepickerConfig>;
    editableCol = '';
    invalidDateMsg = ValidationService.dateInvalidMessage;

    @Input() inputDate: any;
    @Input() uniqueId: string;
    @ViewChild(BsDatepickerDirective, { static: false }) datepicker: BsDatepickerDirective;
    @ViewChild('date', { static: false }) dateElement;
    invalidDateUpdated = false;
    @Output() dateUpdatedEventEmitter = new EventEmitter();

    ngOnInit() {
        this.initialiseDatePicker();
        if (this.inputDate !== null)
            this.inputDate = DateService.convertDateInBainFormat(this.inputDate);
    }

    initialiseDatePicker() {
        this.bsConfig = BS_DEFAULT_CONFIG;
        this.bsConfig.containerClass = 'theme-red calendar-align-right';
    }

    closeForm() {
        this.bsModalRef.hide();
    }

    editDate() {
        // fix fo preventing multiple calls when user keeps clicking inside the same text box
        if (this.editableCol === 'date') {
            return;
        }
        this.editableCol = 'date';
        const _self = this;
        setTimeout(() => {
            if (!_self.datepicker.isOpen) {
                _self.datepicker.show();
            }
            _self.dateElement.nativeElement.select();
            _self.dateElement.nativeElement.focus();
        }, 0);
    }

    onDateChange(date) {
        if (date !== null && !ValidationService.validateDate(date).isValid) {
            this.invalidDateUpdated = true;
        } else {
            this.invalidDateUpdated = false;
            this.dateUpdatedEventEmitter.emit({ employeeCode: this.uniqueId, updatedDate: this.inputDate });
            this.inputDate = DateService.convertDateInBainFormat(date);
        }
        this.disableDateEdit('');
    }

    disableDateEdit(event) {
        const _self = this;
        setTimeout(() => {
            if (event.relatedTarget) {
                event.preventDefault();
                return false;
            }
            _self.editableCol = '';
        }, 200); // DO NOT decrease the time. We need datepicker change to fire first and then disable to occur    
    }

    hideValidationMessage(event) {
        this.invalidDateUpdated = false;
        event.stopPropagation();
    }
}
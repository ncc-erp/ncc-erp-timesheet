import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Optional, Inject } from '@angular/core';
import * as moment from 'moment';

@Component({
  selector: 'app-popup',
  templateUrl: './popup.component.html',
  styleUrls: ['./popup.component.css']
})
export class PopupComponent implements OnInit {
  formPopup: FormGroup
  
  constructor(
    private fb: FormBuilder,
    private _dialogRef: MatDialogRef<PopupComponent>,
    @Optional() @Inject(MAT_DIALOG_DATA) private data: any
  ) { }

  ngOnInit() {
    this.formPopup = this.fb.group({
      fromDateCustomTime: ['', Validators.required],
      toDateCustomTime: ['', Validators.required]
    })
  }
  
  submit() {
    if(!this.formPopup.valid) {
      return;
    }
    this.close(true, {fromDateCustomTime: moment(this.formPopup.value.fromDateCustomTime), toDateCustomTime: moment(this.formPopup.value.toDateCustomTime) });
  }

  close(result: any, data?: any): void {
    this._dialogRef.close({result, data});
  }
}

import { Component, Inject, OnInit, Optional } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import * as moment from 'moment';

@Component({
  selector: 'app-popup-custome-time',
  templateUrl: './popup-custome-time.component.html',
  styleUrls: ['./popup-custome-time.component.css']
})
export class PopupCustomeTimeComponent implements OnInit {
  formPopup: FormGroup
  constructor(
    private fb: FormBuilder,
    private _dialogRef: MatDialogRef<PopupCustomeTimeComponent>,
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
    this.close(true, {
      fromDateCustomTime: moment(this.formPopup.value.fromDateCustomTime),
      toDateCustomTime: moment(this.formPopup.value.toDateCustomTime)
    });
  }

  close(result: any, data?: any): void {
    this._dialogRef.close({result, data});
  }

}

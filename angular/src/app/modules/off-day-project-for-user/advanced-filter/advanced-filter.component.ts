import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Component({
  selector: 'app-advanced-filter',
  templateUrl: './advanced-filter.component.html',
  styleUrls: ['./advanced-filter.component.css']
})
export class AdvancedFilterComponent implements OnInit {
  APP_CONSTANT = APP_CONSTANT;
  parent: any;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<AdvancedFilterComponent>
  ) {}

  ngOnInit() {
    this.parent = this.data;
  }

  onFilter() {
    this.parent.onFilter();
    this.dialogRef.close();
  }

  onCancel() {
    this.dialogRef.close();
  }
}

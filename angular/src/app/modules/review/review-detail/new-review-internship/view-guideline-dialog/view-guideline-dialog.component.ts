import { Component, OnInit, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AppComponentBase } from '@shared/app-component-base';
@Component({
  selector: 'app-view-guideline-dialog',
  templateUrl: './view-guideline-dialog.component.html',
  styleUrls: ['./view-guideline-dialog.component.css']
})
export class ViewGuidelineDialogComponent implements OnInit {
  public title: string 
  public content={} as ViewGuideLineDialogData
  constructor(@Inject(MAT_DIALOG_DATA) private data: ViewGuideLineDialogData, public _dialogRef: MatDialogRef<ViewGuidelineDialogComponent>) { }
  ngOnInit() {
    this.title = "Guideline for Capability : ";
    this.content = this.data;
  }
}
export interface ViewGuideLineDialogData {
  title: string,
  content: string,
  capabilityName: string,
  guideLine: string
}
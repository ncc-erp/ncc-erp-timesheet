import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { ReviewDetailService } from '@app/service/api/review-detail.service';

@Component({
  selector: 'app-create-pm-note',
  templateUrl: './create-pm-note.component.html',
  styleUrls: ['./create-pm-note.component.css']
})
export class CreatePmNoteComponent implements OnInit {

  public active = true;
  public saving = false;
  public pmNote = '';
  public createPmNoteStatus = EnumCreatePmNoteStatus.Approved;
  constructor(
    public dialogRef: MatDialogRef<CreatePmNoteComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private reviewDetailService: ReviewDetailService
  ) { }

  ngOnInit() {
  }

  close(res): void {
    this.dialogRef.close(res);
  }

  savePmNote(status: number){
    const pmNoteDto = {
      reviewDetailId: this.data.id,
      status: status,
      privateNote: this.pmNote,
    }
    this.reviewDetailService.savePMNote(pmNoteDto).subscribe(res => {
      this.dialogRef.close(this.pmNote);
    })
  }
}
export enum EnumCreatePmNoteStatus {
  Approved = 1
}

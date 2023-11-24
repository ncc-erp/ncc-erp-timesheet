import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { ReviewDetailService } from '@app/service/api/review-detail.service';

@Component({
  selector: 'app-create-interview-note',
  templateUrl: './create-interview-note.component.html',
  styleUrls: ['./create-interview-note.component.css']
})
export class CreateInterviewNoteComponent implements OnInit {
  public active = true;
  public saving = false;
  public interviewNote = '';
  public createInterviewNoteStatus = EnumCreateInterviewNoteStatus.Approved;

  constructor(
    public dialogRef: MatDialogRef<CreateInterviewNoteComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private reviewDetailService: ReviewDetailService
  ) { }

  ngOnInit() {
  }

  close(res): void{
    this.dialogRef.close(res);
  }

  saveInterviewNote(status: number){
    const interviewNoteDto = {
      reviewDetailId: this.data.id,
      status: status,
      privateNote: this.interviewNote,
    }

    this.reviewDetailService.saveInterviewNote(interviewNoteDto).subscribe(res => {
      this.dialogRef.close(this.interviewNote);
    })
  }

}
export enum EnumCreateInterviewNoteStatus {
  Approved = 1
}
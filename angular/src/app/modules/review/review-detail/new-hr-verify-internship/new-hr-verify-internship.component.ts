import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { ReviewDetailService } from '@app/service/api/review-detail.service';

@Component({
  selector: 'app-new-hr-verify-internship',
  templateUrl: './new-hr-verify-internship.component.html',
  styleUrls: ['./new-hr-verify-internship.component.css']
})
export class NewHrVerifyInternshipComponent implements OnInit {

  public active = true;
  public saving = false;
  public noteHrVerify = ''; 
  constructor(
    public dialogRef: MatDialogRef<NewHrVerifyInternshipComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private reviewDetailService: ReviewDetailService,
  ) { }

  close(res): void {
    this.dialogRef.close(res);
  }

  saveHrVerifyIntern(status: number){
    const reviewInternCommentDto = {
      reviewDetailId: this.data.id,
      status: status,
      privateNote: this.noteHrVerify,
    }
    this.reviewDetailService.saveHrVerifyIntern(reviewInternCommentDto).subscribe(res => {
      this.dialogRef.close(this.noteHrVerify);
    });
  }


  ngOnInit() {
  }

}

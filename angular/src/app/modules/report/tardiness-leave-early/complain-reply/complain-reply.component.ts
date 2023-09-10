import { AppComponentBase } from 'shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { Component, OnInit, Inject, Injector } from '@angular/core';

@Component({
  selector: 'app-complain-reply',
  templateUrl: './complain-reply.component.html',
  styleUrls: ['./complain-reply.component.css'],

})
export class ComplainReplyComponent extends AppComponentBase implements OnInit {
  complain = {} as complainDto
  constructor(private timekeepingService: TimekeepingService, injector: Injector,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ComplainReplyComponent>) {
    super(injector)
  }

  ngOnInit() {
    this.complain.id = this.data.timekeepingId
    this.complain.noteReply = this.data.noteReply
    this.complain.statusPunish = this.data.statusPunish
    this.complain.isPunishedCheckIn = this.data.status==0?false:true
  }
  saveAndClose() {
    this.timekeepingService.answerComplain(this.complain).subscribe(data => {
      abp.notify.success("Reply successfully");
      this.dialogRef.close(this.complain)
    })
  }
  cancel() {
    this.dialogRef.close()
  }
  
}
export class complainDto {
  isPunishedCheckIn: boolean;
  noteReply: string;
  statusPunish: number;
  id: number;
}
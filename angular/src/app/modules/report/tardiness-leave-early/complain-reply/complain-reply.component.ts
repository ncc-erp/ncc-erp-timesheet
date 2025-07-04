import { AppComponentBase } from 'shared/app-component-base';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material';
import { TimekeepingService } from '@app/service/api/timekeeping.service';
import { Component, OnInit, AfterViewInit, Inject, Injector, ChangeDetectorRef } from '@angular/core';
import { APP_CONSTANT } from '@app/constant/api.constants';

@Component({
  selector: 'app-complain-reply',
  templateUrl: './complain-reply.component.html',
  styleUrls: ['./complain-reply.component.css'],
})
export class ComplainReplyComponent extends AppComponentBase implements OnInit, AfterViewInit {
  complain = {} as complainDto;
  showChangeCount = false;
  
  readonly PUNISH_TYPES_WITH_COUNT = [6, 7];
  changeCount: number = 0;

  constructor(
    private timekeepingService: TimekeepingService, 
    injector: Injector,
    private cdr: ChangeDetectorRef,
    @Inject(MAT_DIALOG_DATA) public data: any,
    public dialogRef: MatDialogRef<ComplainReplyComponent>
  ) {
    super(injector);
  }

  ngOnInit() {
    this.complain.timekeepingId = this.data.timekeepingId;
    this.complain.userpunishmentId = this.data.userpunishmentId;
    this.complain.noteReply = this.data.noteReply;
    this.complain.statusPunish = this.data.statusPunish;
    this.complain.punishType = this.data.punishType;
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.checkPunishType(this.data.statusPunish);
      this.cdr.detectChanges();
    });
  }

  private checkPunishType(punishType: number) {
    this.showChangeCount = this.PUNISH_TYPES_WITH_COUNT.indexOf(punishType) !== -1;
    if (this.showChangeCount) {
      this.changeCount = this.complain.changeCount !== null && this.complain.changeCount !== undefined 
        ? this.complain.changeCount 
        : 0;
      this.complain.changeCount = this.changeCount;
    } else {
      this.complain.changeCount = null;
    }
  }
  
  onPunishTypeChange() {
    this.checkPunishType(this.complain.statusPunish);
    this.cdr.detectChanges();
  }

  increment() {
    this.changeCount++;
    this.complain.changeCount = this.changeCount;
  }

  decrement() {
    this.changeCount--;
    this.complain.changeCount = this.changeCount;
  }
  saveAndClose() {
    if (this.showChangeCount) {
      this.complain.changeCount = this.changeCount;
    }

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
  noteReply: string;
  statusPunish: number;
  timekeepingId: number;
  userpunishmentId: number;
  changeCount?: number;
  punishType?: number; 
}
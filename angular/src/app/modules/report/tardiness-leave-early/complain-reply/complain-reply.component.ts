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
  filteredPunishTypes: any[] = [];
  
  private readonly PUNISHMENT_GROUPS = APP_CONSTANT.PunishmentGroups;
  private readonly PUNISHMENT_TYPE_MAP = APP_CONSTANT.PunishmentTypeMap;

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

    if (this.PUNISH_TYPES_WITH_COUNT.some(x => x === this.complain.statusPunish)) {
      this.changeCount = this.data.count || 0;
      this.complain.changeCount = this.changeCount;
    }
    
    this.updateFilteredPunishTypes(this.complain.statusPunish);
  }

  ngAfterViewInit() {
    setTimeout(() => {
      this.checkPunishType(this.data.statusPunish);
      this.cdr.detectChanges();
    });
  }

  private checkPunishType(punishType: number) {
    this.showChangeCount = this.PUNISH_TYPES_WITH_COUNT.some(x => x === punishType);
    if (this.showChangeCount) {
      if (this.complain.changeCount === undefined || this.complain.changeCount === null) {
        this.complain.changeCount = this.data.count || 0;
      }
      this.changeCount = this.complain.changeCount;
    } else {
      this.complain.changeCount = null;
    }
  }
  
  onPunishTypeChange() {
    this.checkPunishType(this.complain.statusPunish);
    this.updateFilteredPunishTypes(this.complain.statusPunish);
    this.cdr.detectChanges();
  }

  private updateFilteredPunishTypes(currentType: number) {
    const allowedGroups = this.PUNISHMENT_TYPE_MAP[currentType] || [];
    
    const allowedTypes = new Set<number>();
    allowedGroups.forEach(group => {
      this.PUNISHMENT_GROUPS[group].forEach(type => allowedTypes.add(type));
    });
    
    this.filteredPunishTypes = APP_CONSTANT.PunishRules
      .filter(option => allowedTypes.has(option.value))
      .sort((a, b) => a.value - b.value);
  }

  increment() {
    if (this.PUNISH_TYPES_WITH_COUNT.some(x => x === this.complain.statusPunish)) {
      this.changeCount++;
      this.complain.changeCount = this.changeCount;
    }
  }

  decrement() {
    if (this.PUNISH_TYPES_WITH_COUNT.some(x => x === this.complain.statusPunish) && this.changeCount > 0) {
      this.changeCount--;
      this.complain.changeCount = this.changeCount;
    }
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
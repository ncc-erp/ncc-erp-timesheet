import { Component, Inject, Injector, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UserPunishmentService } from '@app/service/api/user-punishment.service';
import { AppComponentBase } from '@shared/app-component-base';

@Component({
  selector: 'app-get-pm-other-punishment-dialog',
  templateUrl: './get-pm-other-punishment-dialog.component.html',
  styleUrls: ['./get-pm-other-punishment-dialog.component.css']
})
export class GetPMOtherPunishmentDialogComponent extends AppComponentBase implements OnInit {
  getPMOtherPunishmentAtMonth: number;
  getPMOtherPunishmentAtYear: number;

  months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12];
  years: number[] = [];

  constructor(
    @Inject(MAT_DIALOG_DATA) public data,
    public dialogRef: MatDialogRef<GetPMOtherPunishmentDialogComponent>,
    private userPunishmentService: UserPunishmentService,
    injector: Injector
  ) {
    super(injector);
  }
  ngOnInit() {
    const currentDate = new Date();
    const currentYear = currentDate.getFullYear();
    this.getPMOtherPunishmentAtMonth = currentDate.getMonth() + 1;
    this.getPMOtherPunishmentAtYear = currentYear;
    for (let i = currentYear; i > currentYear - 5; i--) {
      this.years.push(i);
    }
  }

  onManualTriggerPMOtherPunishment() {
    if (!this.getPMOtherPunishmentAtMonth || !this.getPMOtherPunishmentAtYear) {
      abp.message.error("Please select a month and year to execute!");
      return;
    }
    abp.message.confirm(
      'Are you sure you want to apply PM Other Punishment?',
      'Confirm',
      (result: boolean) => {
        if (result) {
          this.userPunishmentService.triggerManualPunishment(this.getPMOtherPunishmentAtMonth, this.getPMOtherPunishmentAtYear).subscribe(
            () => {
              abp.notify.success('PM Other Punishment has been applied successfully');
            },
          );
        }
      }
    );
  }

  close(): void {
    this.dialogRef.close();
  }
}
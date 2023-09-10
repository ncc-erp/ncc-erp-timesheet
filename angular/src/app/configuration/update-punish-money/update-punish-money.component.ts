import { AppComponentBase } from 'shared/app-component-base';
import { MatDialogRef } from '@angular/material/dialog';
import { CheckInCheckOutPunishmentSettingService } from './../../service/api/punish-by-rule.service';

import { MAT_DIALOG_DATA } from '@angular/material';
import { Component, OnInit, Inject, Injector } from '@angular/core';
import { InputToUpdateSettingDto } from '../configuration.component';

@Component({
  selector: 'app-update-punish-money',
  templateUrl: './update-punish-money.component.html',
  styleUrls: ['./update-punish-money.component.css']
})
export class UpdatePunishMoneyComponent extends AppComponentBase implements OnInit {

  constructor(
    @Inject(MAT_DIALOG_DATA) public data,
    public dialogRef: MatDialogRef<UpdatePunishMoneyComponent>,
    private punishByRulesService: CheckInCheckOutPunishmentSettingService,
    injector: Injector
  ) {
    super(injector);
  }

  public inputToUpdate = {} as InputToUpdateSettingDto;
  public saving: boolean = false;
  ngOnInit() {
    this.inputToUpdate = this.data;
  }

  onSaveAndClose() {
    this.saving = true;
    this.punishByRulesService.setCheckInCheckOutPunishmentSetting(this.inputToUpdate).subscribe(rs => {
      if (rs) {
        abp.notify.success("Update punish money successfully");
        this.saving = false;
        this.dialogRef.close(true);
      }
    }, () => this.saving = false);
  }
  close(): void {
    this.dialogRef.close();
  }

}

import { AppComponentBase } from '@shared/app-component-base';
import { PunishmentService, PunishmentDto } from '../../../service/api/punishment.service';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { Component, Injector, Inject, OnInit } from '@angular/core';
import { finalize } from 'rxjs/operators';

export interface IPunishmentType {
  value: number;
  name: string;
}

@Component({
  selector: 'app-create-edit-punishment',
  templateUrl: './create-edit-punishment.component.html',
  styleUrls: ['./create-edit-punishment.component.css']
})
export class CreateEditPunishmentComponent extends AppComponentBase implements OnInit {
  punishment = {} as PunishmentDto;
  title: string;
  active = true;
  isSaving = false;
  
  punishmentTypes: any[] = [];
  isLoadingTypes = true;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: PunishmentDto,
    injector: Injector,
    private _punishmentService: PunishmentService,
    private _dialogRef: MatDialogRef<CreateEditPunishmentComponent>
  ) {
    super(injector);
  }

  ngOnInit() {
    this.punishment = this.data || {} as PunishmentDto;
    this.title = this.punishment.id ? 'Edit Punishment: ' : 'New Punishment';
    this.loadPunishmentTypes();
  }

  loadPunishmentTypes() {
    this.isLoadingTypes = true;
    const types = this._punishmentService.getPunishmentTypes();
    if (types && types.length > 0) {
      this.punishmentTypes = types;
      this.isLoadingTypes = false;
    } else {
      setTimeout(() => {
        this.loadPunishmentTypes();
      }, 100);
    }
  }

  save() {
    if (this.isSaving) {
      return;
    }

    this.isSaving = true;
    const request = this.punishment.id 
      ? this._punishmentService.update(this.punishment)
      : this._punishmentService.create(this.punishment);

    request.subscribe(
      (response) => {
        if (response.success) {
          this.notify.success(
            this.punishment.id 
              ? this.l('UpdatePunishmentSuccessfully')
              : this.l('CreatePunishmentSuccessfully')
          );
          this._dialogRef.close(response.result);
        } else {
          this.isSaving = false;
        }
      },
      () => {
        this.isSaving = false;
      }
    );
  }

  close(result?: any): void {
    this._dialogRef.close(result);
  }
}

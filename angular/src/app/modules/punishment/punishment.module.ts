import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PunishmentComponent } from './punishment.component';
import { PunishmentRoutingModule } from './punishment-routing.module';
import { NgxPaginationModule } from 'ngx-pagination';
import { SharedModule } from '@shared/shared.module';
import { CreateEditPunishmentComponent } from './create-edit-punishment/create-edit-punishment.component';
import { ImportErrorDialogComponent } from './import-error-dialog/import-error-dialog.component';

@NgModule({
  declarations: [
    PunishmentComponent,
    CreateEditPunishmentComponent,
    ImportErrorDialogComponent
  ],
  imports: [
    CommonModule,
    PunishmentRoutingModule,
    SharedModule,
    ReactiveFormsModule,
    NgxPaginationModule,
    FormsModule

  ],
  exports: [PunishmentComponent],
  entryComponents: [
    CreateEditPunishmentComponent,
    ImportErrorDialogComponent
  ]
})
export class PunishmentModule { }

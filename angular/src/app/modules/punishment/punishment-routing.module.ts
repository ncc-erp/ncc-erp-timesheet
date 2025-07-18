import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PunishmentComponent } from './punishment.component';

const routes: Routes = [
  { path: '', component: PunishmentComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class PunishmentRoutingModule { }

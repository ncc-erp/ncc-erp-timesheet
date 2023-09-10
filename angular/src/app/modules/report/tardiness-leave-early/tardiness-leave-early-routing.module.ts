import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { TardinessLeaveEarlyComponent } from './tardiness-leave-early.component';

const routes: Routes = [
  { path: '', component: TardinessLeaveEarlyComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TardinessLeaveEarlyRoutingModule { }

import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { LeaveDayOfUserComponent } from './leave-day-of-user.component';

const routes: Routes = [{
  path: '',
  component: LeaveDayOfUserComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LeaveDayOfUserRoutingModule { }
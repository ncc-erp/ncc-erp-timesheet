import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ViewLeaveDayOfUserComponent } from './view-leave-day-of-user.component';

const routes: Routes = [{
  path: '',
  component: ViewLeaveDayOfUserComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ViewLeaveDayOfUserRoutingModule { }

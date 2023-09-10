import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { OffDayProjectForUserComponent } from './off-day-project-for-user.component';

const routes: Routes = [{
  path: '',
  component: OffDayProjectForUserComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OffDayProjectForUserRoutingModule { }

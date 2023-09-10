import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ManageWorkingTimesComponent } from './manage-working-times.component';

const routes: Routes = [{
  path : '',
  component: ManageWorkingTimesComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ManageWorkingTimesRoutingModule { }

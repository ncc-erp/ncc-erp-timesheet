import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { ManageAbsenceTypesComponent } from './manage-absence-types.component';

const routes: Routes = [{
  path : '',
  component: ManageAbsenceTypesComponent
}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ManageAbsenceTypesRoutingModule { }

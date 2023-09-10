import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { OffDayProjectComponent } from './off-day-project.component';

const routes: Routes = [{
  path: '',
  component: OffDayProjectComponent
}]
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class OffDayProjectRoutingModule { }
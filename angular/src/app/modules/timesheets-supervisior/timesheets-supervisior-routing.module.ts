import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TimesheetsSupervisiorComponent } from './timesheets-supervisior.component';

const routes: Routes = [
    {
      path: '',
      component: TimesheetsSupervisiorComponent
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
  })
  export class TimesheetsSupervisiorRoutingModule { }

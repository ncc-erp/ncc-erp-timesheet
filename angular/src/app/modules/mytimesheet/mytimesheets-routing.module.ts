import { MyTimeSheetsComponent } from './mytimesheets.component';
import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
const routes: Routes = [
    {
      path: '',
      component: MyTimeSheetsComponent
    }
]
@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
  })
  export class MyTimeSheetsRoutingModule { }

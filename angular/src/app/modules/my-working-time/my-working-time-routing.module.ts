import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { MyWorkingTimeComponent } from './my-working-time.component';

const routes: Routes = [
  {path: '', component: MyWorkingTimeComponent}
];

@NgModule({
  imports: [
    RouterModule.forChild(routes)
  ],
  exports: [RouterModule]
})
export class MyWorkingTimeRoutingModule { }

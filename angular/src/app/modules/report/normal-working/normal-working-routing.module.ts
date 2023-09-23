import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { NormalWorkingComponent } from './normal-working.component';

const routes: Routes = [
  { path: '', component: NormalWorkingComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class NormalWorkingRoutingModule {
}

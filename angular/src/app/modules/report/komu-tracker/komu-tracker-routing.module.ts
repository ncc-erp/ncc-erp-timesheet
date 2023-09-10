import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { KomuTrackerComponent } from './komu-tracker.component';

const routes: Routes = [
  { path: '', component: KomuTrackerComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class KomuTrackerRoutingModule { }

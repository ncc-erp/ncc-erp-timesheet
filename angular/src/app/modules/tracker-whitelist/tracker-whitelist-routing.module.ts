import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TrackerWhitelistComponent } from './tracker-whitelist.component';

const routes: Routes = [
    {
        path: '',
        component: TrackerWhitelistComponent
    }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class TrackerWhitelistRoutingModule { }
import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { WhitelistSystemComponent } from './whitelist-system.component';

const routes: Routes = [
    {
        path: '',
        component: WhitelistSystemComponent
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class WhitelistSystemRoutingModule { }
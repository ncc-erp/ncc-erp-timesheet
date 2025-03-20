import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { LoginComponent } from './login/login.component';
import { RegisterComponent } from './register/register.component';
import { AccountComponent } from './account.component';
import { AuthCallbackComponent } from './auth-callback/auth-callback.component';
import { MezonhashCallbackComponent } from './mezon-hash/mezonhash-callback.component';
@NgModule({
    imports: [
        RouterModule.forChild([
            {
                path: '',
                component: AccountComponent,
                children: [
                    { path: 'login', component: LoginComponent },
                    { path: 'register', component: RegisterComponent },
                ]
            },
            { path: 'login/callback', component: AuthCallbackComponent },
            { path: 'mezonhash-call', component: MezonhashCallbackComponent }

        ])
    ],
    exports: [
        RouterModule
    ]
})
export class AccountRoutingModule { }

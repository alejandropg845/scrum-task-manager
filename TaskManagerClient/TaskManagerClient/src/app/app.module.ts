import { LOCALE_ID, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';


import { AppComponent } from './app.component';
import { AppRoutingModule } from './app.routes';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { PopupResponseComponent } from './shared/popup-response/popup-response.component';
import { tokenInterceptor } from './services/token.interceptor';
import { SidebarComponent } from './shared/sidebar/sidebar.component';
import { CommonModule, registerLocaleData } from '@angular/common';
import { AssignedTasksComponent } from './shared/assigned-tasks/assigned-tasks.component';
import { MyTasksComponent } from './pages/tasks/tasks.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { GroupMembersComponent } from './pages/group-members/group-members.component';
import { UserComponent } from './pages/user/user.component';
import { ReactiveFormsModule } from '@angular/forms';
import { AddTaskComponent } from './pages/add-task/add-task.component';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { OAuthModule } from 'angular-oauth2-oidc';
import { MainComponent } from './pages/main/main.component';
import { PipeIsYou } from './pipes/isYou.pipe';
import { ChatComponent } from './pages/chat/chat.component';
import { AvatarPipe } from './pipes/avatar.pipe';
import { SprintsComponent } from './pages/sprints/sprints.component';
import { RetrospectivesComponent } from './pages/retrospectives/retrospectives.component';
import localeEs  from '@angular/common/locales/es';

registerLocaleData(localeEs)
@NgModule({
  declarations: [
    AppComponent,
    PopupResponseComponent,
    SidebarComponent,
    AssignedTasksComponent,
    MyTasksComponent,
    LoginComponent,
    RegisterComponent,
    GroupMembersComponent,
    UserComponent,
    AddTaskComponent,
    ForgotPasswordComponent,
    MainComponent,
    PipeIsYou,
    AvatarPipe,
    ChatComponent,
    SprintsComponent,
    RetrospectivesComponent
  ],
  imports: [
    CommonModule,
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    OAuthModule.forRoot()
  ],
  providers: [provideHttpClient(withInterceptors([tokenInterceptor])), { provide: LOCALE_ID, useValue: 'es' }],
  bootstrap: [AppComponent]
})
export class AppModule { }

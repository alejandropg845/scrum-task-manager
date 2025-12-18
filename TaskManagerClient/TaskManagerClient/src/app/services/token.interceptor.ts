import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { TasksService } from './tasks.service';
import { catchError, switchMap, throwError } from 'rxjs';
import { PopupService } from './popup.service';
import { UsersHubService } from './hub.service';
import { UserService } from './users.service';
import { AuthService } from './auth.service';
import { TokenService } from './token.service';

export const tokenInterceptor: HttpInterceptorFn = (req, next) => {

  const tasksService = inject(TasksService);
  const userService = inject(UserService);
  const popupService = inject(PopupService);
  const usersHub = inject(UsersHubService);
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const httpClient = inject(HttpClient);
  let clonedRequest = req;

  let currentRequest = null;

  if(tasksService.getToken){
    

    clonedRequest = req.clone({
      setHeaders:{
        Authorization: `bearer ${tasksService.getToken}`
      }
    });
  }

  return next(clonedRequest).pipe
  (
    catchError((err:HttpErrorResponse) => {

      if(err.status !== 401) {
        return throwError(() => err);
      }
      const refreshToken = localStorage.getItem('tmrt');
      return tokenService.getNewAccessToken(refreshToken!)
      .pipe(

        switchMap((res: {accessToken:string, refreshToken:string}) => {

        localStorage.setItem('tmat', res.accessToken);
        localStorage.setItem('tmrt', res.refreshToken)
        tasksService.refreshToken = res.refreshToken;
        tasksService.accessToken = res.accessToken;
        const newRequest = req.clone({
          setHeaders: {
            Authorization: `bearer ${res.accessToken}`
          }
        });

        return next(newRequest);

        }),
        catchError((refreshError:HttpErrorResponse) => {
          deleteUserInfo(userService, tasksService);
          return throwError(() => refreshError);
        })
      );

    })
  );
};

function deleteUserInfo(userService:UserService, tasksService:TasksService){
  tasksService.accessToken = null;
  localStorage.removeItem('tmat');
  localStorage.removeItem('tmrt');
  userService.groupName = null;
  userService.username = null;
  userService.expirationTime = null;
  userService.groupRole = null;
  userService.isGroupOwner = false;
  userService.isAllowed = false;
  userService.status = "";
  userService.userPendingTasks.next([]);
  userService.usersInGroup_BSubject.next([]);
  userService.isScrum = false;
}

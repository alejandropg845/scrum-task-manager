import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { AuthConfig, OAuthService } from "angular-oauth2-oidc";
import { filter, Subject } from "rxjs";
import { ApiGatewayService } from "./api-gateway.service";

@Injectable({
    providedIn: 'root'
})
export class AuthService {

    loginUser(username:string, password:string){

        const body = {
            username, password
        }

        return this.http.post<{ok:boolean, accessToken:string, refreshToken:string, message:string}>(`${environment.usersUrl}/api/users/LoginUser`, body);
    }
    
    registerUser(username:string, password:string, email:string){

        const body = {
            username, password, email
        }

        return this.http.post<{ok:boolean, accessToken:string, refreshToken:string, message:string}>(`${environment.usersUrl}/api/users/RegisterUser`, body);

    }

    forgotPassword(email:string){
        return this.apiGateway.sendRequest("post", "users", `RecoverPassword/${email}`);
    }

    receiveRecoveryCode(recoveryCode:string, password1:string, password2:string, email:string){

        const body = { recoveryCode, password1, password2, email };
        
        return this.apiGateway.sendRequest<any>("post", "users", `ReceiveRecoveryCode`, body);
    }
    
    sendGoogleInfo_Subject = new Subject<string>();

    initiateGoogleConfig = new Subject<boolean>();

    getOAuthConfig() {
        
        this.initiateGoogleConfig.asObservable()
        .subscribe(() => {
            const oauthConfig: AuthConfig = {
            issuer: 'https://accounts.google.com',
            strictDiscoveryDocumentValidation: false,
            redirectUri: window.location.origin,
            clientId: environment.googleClientId,
            scope: 'openid profile email'
            };

            this.oAuthService.configure(oauthConfig);

            this.oAuthService.loadDiscoveryDocumentAndTryLogin();

            this.oAuthService.events
            .pipe(filter((e) => e.type === "token_received"))
            .subscribe(_ => {

                const idToken =  this.oAuthService.getIdToken();
                this.sendGoogleInfo_Subject.next(idToken);
                this.logOutGoogle();
            });

        });

    }

    sendGoogleInfoForLogin(tokenId:string){
        return this.http.post<{ok:boolean, accessToken:string, refreshToken:string, message:string}>(`${environment.usersUrl}/api/users/ContinueWithGoogle/${tokenId}`, null);
    }

    continueWithOAuthGoogle(){
        this.oAuthService.initLoginFlow();
    }

    logOutGoogle(){
        this.oAuthService?.logOut();
    }

    constructor(private http:HttpClient, private oAuthService:OAuthService, private apiGateway:ApiGatewayService){
        this.getOAuthConfig();
    }

}
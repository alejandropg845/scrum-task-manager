import { Pipe, PipeTransform } from "@angular/core";
import { UserService } from "../services/users.service";

@Pipe({
    name: 'isYou'
})
export class PipeIsYou implements PipeTransform {

    transform(username:string) {
        const isYou = username === this.userService.username;
        return isYou ? `${username} (Tú)` : username
    }

    constructor(private userService:UserService){}
}
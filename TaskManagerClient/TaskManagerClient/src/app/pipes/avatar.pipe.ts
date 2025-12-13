import { Pipe, PipeTransform } from "@angular/core";

@Pipe({
    name: 'avatar'
})
export class AvatarPipe implements PipeTransform {
    transform(username:string) {
        return username.charAt(0);
    }
}
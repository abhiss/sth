export const ModuleLoadTest = 'module loaded: toastNotify';


let toast_notification_queue = [];

export async function addToastNotification(message, display_time=5000){
    toast_notification_queue.push({message, display_time});
    if(!is_tn_q_handler_in_progress){
        toast_notification_queue_handler();
    }

}

let is_tn_q_handler_in_progress = false;
//recursively handles toast_notification_queue
async function toast_notification_queue_handler(){
    //avoid race
    // if(is_tn_q_handler_in_progress){
    //     console.log("An error occured: toast_notification_queue_handler was called while in progress.");
    //     return;
    // }
    if(toast_notification_queue.length == 0){
        is_tn_q_handler_in_progress = false;
        return;
    }
    else {
        is_tn_q_handler_in_progress = true;
        const data = toast_notification_queue.shift(); //pop is for stacks 

        await show_toast_notification(data.message, data.display_time);
        await new Promise(r => setTimeout(r, 1000))
        toast_notification_queue_handler();
    }
}

const openAnimation = [
    { transform: 'scaleY(0) scaleX(0)' },
    { transform: 'scaleY(1) scaleX(0.01)', offset: 0.3, easing: 'ease-in' },
    { transform: 'scaleX(0.01)', offset: 0.3, easing: 'ease-out' },
    { transform: 'scaleX(1)', offset: 1 },
];
//.finished is a promise
//private function
async function show_toast_notification(message, display_time) {
    const toast_message_elm = document.getElementById('toast_message');
    const toast_message_text_elm = document.getElementById('toast_message_text');

    toast_message_text_elm.innerText = message;

    //open black box
    const toastAnimation = await toast_message_elm.animate(
        openAnimation, {
        duration: 1000, // 1s
        iterations: 1, // single iteration
        easing: 'ease-in', // easing function
        fill:'both'
    }).finished;

    //fade in text
    await toast_message_text_elm.animate(
        //  Inline Keyframes
        [
            { opacity: '0' },
            { opacity: '1' }
        ],
        //  Inline Settings
        {
            duration: 500,
            fill: 'both'
        }
    ).finished

    //display message timer
    await new Promise(r => setTimeout(r, display_time))

    //fade out text
    await toast_message_text_elm.animate(
        [
            { opacity: '1' },
            { opacity: '0' }
        ],
        {
            duration: 250,
            fill: 'both'
        } 
    ).finished
    
    //shrink/hide box
    await toast_message_elm.animate(
        [
            { transform: 'scaleX(1)', offset: 0 },
            { transform: 'scaleY(1) scaleX(0.01)', offset: 0.7, easing: 'ease-in' },
            { transform: 'scaleY(0) scaleX(0)', offset: 1 }
        ],
        {
            duration: 500,
            fill: 'both'
        }
    ).finished
}

// toastAnimation.finished.then(() => {
//     document.getElementById('toast_message_text').animate(
//         //  Inline Keyframes
//         [
//             { opacity: '0' },
//             { opacity: '1' }
//         ],
//         //  Inline Settings
//         {
//             duration: 500,
//             fill: 'both'
//         }
//     )
// }).then(() => {
//     return new Promise(r => setTimeout(r, 5000))
// }).then(() => {
//     document.getElementById('toast_message_text').animate(
//         //  Inline Keyframes
//         [
//             { opacity: '1' },
//             { opacity: '0' }
//         ],
//         //  Inline Settings
//         {
//             duration: 250,
//             fill: 'both'
//         }
//     ).finished.then(() => {
//         document.getElementById('toast_message').animate(
//             [
//                 { transform: 'scaleX(1)', offset: 0 },
//                 { transform: 'scaleY(1) scaleX(0.01)', offset: 0.7, easing: 'ease-in' },
//                 { transform: 'scaleY(0) scaleX(0)', offset: 1 }
//             ],
//             {
//                 duration: 500,
//                 fill: 'both'
//             }
//         )
//     })
// })

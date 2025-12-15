
async function getElement(selector, timeout = 10_000, interval = 100, root = document) {
    const start = performance.now();

    return new Promise((resolve, reject) => {
        const check = () => {
            const el = root.querySelector(selector);
            if (el) {
                resolve(el);
                return;
            }

            if (performance.now() - start >= timeout) {
                reject(new Error(`Timeout: Element "${selector}" not found within ${timeout}ms.`));
                return;
            }

            setTimeout(check, interval);
        };

        check();
    });
}

async function waitStable(fn, duration = 1000, timeout = 20_000) {
    const sleep = (ms) => new Promise(resolve => setTimeout(resolve, ms));
    const pollInterval = Math.min(100, duration / 2);

    let lastValue = undefined;
    let stableStartTime = null;
    const startTime = Date.now();

    while (Date.now() - startTime < timeout) {
        let currentValue;
        try {
            currentValue = await fn();
        } catch (error) {
            throw new Error(`Function execution failed: ${error.message}`);
        }
        if (currentValue !== lastValue) {
            lastValue = currentValue;
            stableStartTime = null;
        } else {
            if (stableStartTime === null) {
                stableStartTime = Date.now();
            } else if (Date.now() - stableStartTime >= duration) {
                return currentValue;
            }
        }
        await sleep(pollInterval);
    }

    throw new Error(`Wait for stability timed out after ${timeout}ms. Last value seen: ${lastValue}`);
}


async function waitUntil(fn, timeout = 20_000, interval = 100) {
    const start = Date.now();
    return new Promise((resolve, reject) => {
        const check = async () => {
            try {
                const result = await fn();
                if (result) {
                    resolve();
                } else if (Date.now() - start >= timeout) {
                    reject(new Error('waitUntil: timeout'));
                } else {
                    setTimeout(check, interval);
                }
            } catch (err) {
                reject(err);
            }
        };
        check();
    });
}

function fireEnterKey(targetElement) {
    if (!targetElement) return;

    // Create a KeyboardEvent for "keydown"
    const event = new KeyboardEvent('keydown', {
        key: 'Enter',
        code: 'Enter',
        keyCode: 13, // deprecated but used for compatibility
        which: 13,   // deprecated but used for compatibility
        bubbles: true,
        cancelable: true
    });

    targetElement.dispatchEvent(event);

    // Optional: also fire "keypress" and "keyup" if needed
    const eventPress = new KeyboardEvent('keypress', { ...event });
    targetElement.dispatchEvent(eventPress);

    const eventUp = new KeyboardEvent('keyup', { ...event });
    targetElement.dispatchEvent(eventUp);
    console.log(`Dispatched Enter key events on ${targetElement.tagName}`);
}

function sleep(ms) {
    return new Promise(resolve => {
        setTimeout(resolve, ms);
    });
}

async function ask(question) {
    const getContainers = () => document.querySelectorAll('message-content')
    const count = getContainers().length
    const input = await getElement('.ql-editor>p')
    input.textContent = question
    fireEnterKey(input)
    //wait for answer
    await waitUntil(() => {
        return getContainers().length > count
    })
    //wait for it to finish
    await waitStable(() => {
        return document.querySelectorAll('[data-path-to-node]').length
    }, 1000)
    await sleep(1000)
    const containers = getContainers()
    const lastContainer = containers[containers.length - 1];
    const answer = lastContainer.innerText
    //console.log('answer', answer)
    return answer
}

async function askAI(payload) {
    const { question } = payload
    return await ask(question)
}

async function main() {
    //const input = await getElement('.ql-editor>p')
    //ask('hello')
}

main()
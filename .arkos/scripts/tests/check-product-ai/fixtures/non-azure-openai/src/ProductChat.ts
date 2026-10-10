export async function complete(prompt: string): Promise<string> {
  const response = await fetch("https://api.openai.com/v1/chat/completions", {
    method: "POST",
    body: JSON.stringify({ model: "gpt-4o", messages: [{ role: "user", content: prompt }] }),
  });
  return response.text();
}
